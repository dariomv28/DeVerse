import { Injectable } from "@nestjs/common"
import { InjectModel } from "@nestjs/mongoose"
import { Model } from "mongoose"
import { BadRequestException } from "@nestjs/common"
import { FriendRequest, FriendRequestDocument } from "./schemas/friend_request.schema"
import { NotFoundException, ForbiddenException } from "@nestjs/common"
import { UserDocument } from "./schemas/user.schema"
import { User } from "./schemas/user.schema"
import { UserGateway } from './user.gateway'

@Injectable()
export class FriendRequestService {
    constructor(
        @InjectModel(FriendRequest.name)
        private friendRequestModel: Model<FriendRequestDocument>,

        @InjectModel(User.name)
        private userModel: Model<UserDocument>,
        private userGateway: UserGateway,
    ) { }

    async createRequest(from: string, to: string) {
        const fromStr = from && typeof from !== 'string' && (from as any).toString ? (from as any).toString() : (from || '')
        const toStr = to && typeof to !== 'string' && (to as any).toString ? (to as any).toString() : (to || '')

        if (fromStr === toStr) {
            throw new BadRequestException('Cannot add yourself')
        }
        // check sent
        const existing = await this.friendRequestModel.findOne({
            from: fromStr,
            to: toStr,
            status: 'pending',
        })
        if (existing) {
            throw new BadRequestException('Request already sent') // shoud do return request later
        }
        const created = await this.friendRequestModel.create({
            from: fromStr,
            to: toStr,
        })
        // emit websocket event
        try {
          this.userGateway.emitFriendRequest({ from: fromStr, to: toStr, requestId: created._id })
        } catch (e) {}
        return created
    }
    async getRequests(userId: string) {
        const userIdStr = userId && typeof userId !== 'string' && (userId as any).toString ? (userId as any).toString() : (userId || '')
        return this.friendRequestModel
            .find({ to: userIdStr, status: 'pending' })
            .populate('from', 'name email avatar')
    }
    async acceptRequest(requestId: string, currentUserId: string) {
        const request = await this.friendRequestModel.findById(requestId)

        if (!request) {
            throw new NotFoundException('Request not found')
        }

        const currentUserIdStr = currentUserId && typeof currentUserId !== 'string' && (currentUserId as any).toString ? (currentUserId as any).toString() : (currentUserId || '')

        if (request.to.toString() !== currentUserIdStr) {
            throw new ForbiddenException('Not allowed')
        }

        if (request.status !== 'pending') {
            throw new BadRequestException('Already handled')
        }

        request.status = 'accepted'
        await request.save()

        // add friend 2 chiều
        await this.userModel.findByIdAndUpdate(request.from, {
            $addToSet: { friends: request.to },
        })

        await this.userModel.findByIdAndUpdate(request.to, {
            $addToSet: { friends: request.from },
        })

        try {
          this.userGateway.emitFriendAccepted({ from: request.from.toString(), to: request.to.toString(), requestId: request._id })
        } catch (e) {}

        return { message: 'Accepted' }
    }
    async rejectRequest(requestId: string, currentUserId: string) {
        const request = await this.friendRequestModel.findById(requestId)

        if (!request) {
            throw new NotFoundException('Request not found')
        }

        const currentUserIdStr = currentUserId && typeof currentUserId !== 'string' && (currentUserId as any).toString ? (currentUserId as any).toString() : (currentUserId || '')

        if (request.to.toString() !== currentUserIdStr) {
            throw new ForbiddenException('Not allowed')
        }

        request.status = 'rejected'
        await request.save()

        try {
            this.userGateway.emitFriendRequestCancelled({ from: request.from.toString(), to: request.to.toString(), requestId: request._id })
        } catch (e) {}

        return { message: 'Rejected' }
    }

        async cancelRequest(from: string, to: string) {
            const fromStr = from && typeof from !== 'string' && (from as any).toString ? (from as any).toString() : (from || '')
            const toStr = to && typeof to !== 'string' && (to as any).toString ? (to as any).toString() : (to || '')

            const request = await this.friendRequestModel.findOne({ from: fromStr, to: toStr, status: 'pending' })
            if (!request) {
                throw new NotFoundException('Request not found')
            }

            await this.friendRequestModel.findByIdAndDelete(request._id)

            try {
                this.userGateway.emitFriendRequestCancelled({ from: fromStr, to: toStr, requestId: request._id })
            } catch (e) {}

            return { message: 'Cancelled' }
        }
}