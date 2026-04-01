import { Injectable } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { User, UserDocument } from './schemas/user.schema';
import { Model } from 'mongoose';
import { BadRequestException } from '@nestjs/common';

@Injectable()
export class UserService {
    constructor(
        @InjectModel(User.name)
        private userModel: Model<UserDocument>
    ) {}
    async findAll() {
        return this.userModel.find();
    }
    async findById(id: string) {
        return this.userModel.findById(id);
    }
    async findByEmail(email: string) {
        return this.userModel.findOne({email});
    }
    async create(data: any) {
        return this.userModel.create(data);
    }
    async changePassword(email: string, password: string) {
        return this.userModel.findOneAndUpdate({email}, {password});
    }
    async search(q: string, currentUserId: string) {
        return this.userModel.find({
            _id: {$ne: currentUserId},
            name: {$regex: q, $options: 'i'},
        }).limit(5)
    }
    async follow(userId: string, targetId: string) {
        if (userId === targetId) {
            throw new BadRequestException('Cannot follow yourself')
        }
        await this.userModel.findByIdAndUpdate(userId, {
            $addToSet: { following: targetId },
        })
        await this.userModel.findByIdAndUpdate(targetId, {
            $addToSet: { followers: userId },
        })
        return { message: 'Followed' }
    }
}