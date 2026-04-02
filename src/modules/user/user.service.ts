import { Injectable, BadRequestException } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { User, UserDocument } from './schemas/user.schema';
import { Model } from 'mongoose';
import { ConfigService } from '@nestjs/config';
import { Multer } from 'multer';

@Injectable()
export class UserService {
    constructor(
        @InjectModel(User.name)
        private userModel: Model<UserDocument>,

        private configService: ConfigService,
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
        return this.userModel
            .find({
                _id: { $ne: currentUserId },
                name: { $regex: q, $options: 'i' },
            })
            .limit(5);
    }

    async follow(userId: string, targetId: string) {
        if (userId === targetId) {
            throw new BadRequestException('Cannot follow yourself');
        }
        await Promise.all([
            this.userModel.findByIdAndUpdate(userId, {
                $addToSet: { following: targetId },
            }),
            this.userModel.findByIdAndUpdate(targetId, {
                $addToSet: { followers: userId },
            }),
        ]);
        return { message: 'Followed' };
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