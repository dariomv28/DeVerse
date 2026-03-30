import { Injectable } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { User, UserDocument } from './schemas/user.schema';
import { Model } from 'mongoose';

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
}