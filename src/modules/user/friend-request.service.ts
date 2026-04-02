import { Injectable } from "@nestjs/common"
import { InjectModel } from "@nestjs/mongoose"
import { Model } from "mongoose"
import { BadRequestException } from "@nestjs/common"
import { FriendRequest, FriendRequestDocument } from "./schemas/friend_request.schema"
import { NotFoundException, ForbiddenException } from "@nestjs/common"
import { UserDocument } from "./schemas/user.schema"
import { User } from "./schemas/user.schema"

@Injectable()
export class FriendRequestService {
    constructor(
        @InjectModel(FriendRequest.name)
        private friendRequestModel: Model<FriendRequestDocument>,

        @InjectModel(User.name)
        private userModel: Model<UserDocument>,
    ) { }

    async createRequest(from: string, to: string) {
        if (from === to) {
            throw new BadRequestException('Cannot add yourself')
        }
        // check sent
        const existing = await this.friendRequestModel.findOne({
            from,
            to,
            status: 'pending',
        })
        if (existing) {
            throw new BadRequestException('Request already sent') // shoud do return request later
        }
        return this.friendRequestModel.create({
            from,
            to,
        })
    }
    async getRequests(userId: string) {
        return this.friendRequestModel
            .find({ to: userId, status: 'pending' })
            .populate('from', 'name email avatar')
    }
    async acceptRequest(requestId: string, currentUserId: string) {
        const request = await this.friendRequestModel.findById(requestId)

        if (!request) {
            throw new NotFoundException('Request not found')
        }

        if (request.to.toString() !== currentUserId) {
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

        return { message: 'Accepted' }
    }
    async rejectRequest(requestId: string, currentUserId: string) {
        const request = await this.friendRequestModel.findById(requestId)

        if (!request) {
            throw new NotFoundException('Request not found')
        }

        if (request.to.toString() !== currentUserId) {
            throw new ForbiddenException('Not allowed')
        }

        request.status = 'rejected'
        await request.save()

        return { message: 'Rejected' }
    }
}