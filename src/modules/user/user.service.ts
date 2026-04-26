import { Injectable, BadRequestException } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { User, UserDocument } from './schemas/user.schema';
import { Model } from 'mongoose';
import { ConfigService } from '@nestjs/config';
import { Multer } from 'multer';
import { FriendRequest, FriendRequestDocument } from './schemas/friend_request.schema';
import { UserGateway } from './user.gateway';

@Injectable()
export class UserService {
    constructor(
        @InjectModel(User.name)
        private userModel: Model<UserDocument>,

        @InjectModel(FriendRequest.name)
        private friendRequestModel: Model<FriendRequestDocument>,

        private configService: ConfigService,

        private userGateway: UserGateway,
    ) { }
    async findById(id: string) {
        return this.userModel.findById(id);
    }
    async findByEmail(email: string) {
        return this.userModel.findOne({ email });
    }

    async create(data: Partial<User>) {
        return this.userModel.create(data);
    }

    async changePassword(email: string, password: string) {
        return this.userModel.findOneAndUpdate(
            { email },
            { password },
            { new: true },
        );
    }

    async search(q: string, currentUserId: string) {
        if (!q || q.trim() === '') return []

        const regex = new RegExp(q, 'i')

        const currentUserIdStr = currentUserId && typeof currentUserId !== 'string' && (currentUserId as any).toString ? (currentUserId as any).toString() : (currentUserId || '')

        const currentUser = await this.userModel.findById(currentUserIdStr).select('following friends').lean()

        const users = await this.userModel
            .find({
                _id: { $ne: currentUserIdStr },
                $or: [{ name: regex }, { email: regex }],
            })
            .select('name email avatar')
            .limit(10)
            .lean()

        const targetIds = users.map((u: any) => u._id)

        const pending = await this.friendRequestModel
            .find({
                status: 'pending',
                $or: [
                    { from: currentUserIdStr, to: { $in: targetIds } },
                    { from: { $in: targetIds }, to: currentUserIdStr },
                ],
            })
            .lean()

        const requestMap = new Map<string, { type: 'sent' | 'received'; id: string }>()
        for (const r of pending) {
            const from = r.from.toString()
            const to = r.to.toString()
            if (from === currentUserIdStr) requestMap.set(to, { type: 'sent', id: r._id.toString() })
            else if (to === currentUserIdStr) requestMap.set(from, { type: 'received', id: r._id.toString() })
        }

        return users.map((u: any) => {
            const id = u._id.toString()
            const isFollowing = Array.isArray(currentUser?.following) && currentUser.following.find((f: any) => f.toString() === id)
            const isFriend = Array.isArray(currentUser?.friends) && currentUser.friends.find((f: any) => f.toString() === id)
            const req = requestMap.get(id)
            return {
                _id: id,
                name: u.name,
                email: u.email,
                avatar: u.avatar || '',
                isFollowing: !!isFollowing,
                isFriend: !!isFriend,
                friendRequestStatus: req ? req.type : 'none',
                friendRequestId: req ? req.id : null,
            }
        })
    }

    async follow(userId: string, targetId: string) {
        if (userId === targetId) {
            throw new BadRequestException('Cannot follow yourself');
        }
        const userIdStr = userId && typeof userId !== 'string' && (userId as any).toString ? (userId as any).toString() : (userId || '')
        const targetIdStr = targetId && typeof targetId !== 'string' && (targetId as any).toString ? (targetId as any).toString() : (targetId || '')

        const me = await this.userModel.findById(userIdStr).select('following').lean()
        const isFollowing = Array.isArray(me?.following) && me.following.find((f: any) => f.toString() === targetIdStr)

        if (isFollowing) {
            await Promise.all([
                this.userModel.findByIdAndUpdate(userIdStr, {
                    $pull: { following: targetIdStr },
                }),
                this.userModel.findByIdAndUpdate(targetIdStr, {
                    $pull: { followers: userIdStr },
                }),
            ])

            try {
                this.userGateway.emitFollow({ from: userIdStr, to: targetIdStr, following: false })
            } catch (e) {}

            return { message: 'Unfollowed', following: false };
        }
        await Promise.all([
            this.userModel.findByIdAndUpdate(userIdStr, {
                $addToSet: { following: targetIdStr },
            }),
            this.userModel.findByIdAndUpdate(targetIdStr, {
                $addToSet: { followers: userIdStr },
            }),
        ])

        try {
            this.userGateway.emitFollow({ from: userIdStr, to: targetIdStr, following: true })
        } catch (e) {}

        return { message: 'Followed', following: true };
    }

    async uploadAvatar(userId: string, file: Express.Multer.File) {
        const baseUrl = this.configService.get<string>('BASE_URL');
        const avatarUrl = `${baseUrl}/uploads/avatars/${file.filename}`;
        return this.userModel.findByIdAndUpdate(
            userId,
            { avatar: avatarUrl },
            { new: true },
        );
    }
}