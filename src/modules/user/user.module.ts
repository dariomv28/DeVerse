import { Module } from '@nestjs/common';
import { UserController } from './user.controller';
import { UserService } from './user.service';
import {User, UserSchema} from './schemas/user.schema';
import {FriendRequest, FriendRequestSchema} from './schemas/friend_request.schema'
import { Mongoose } from 'mongoose';
import { MongooseModule, Schema } from '@nestjs/mongoose';
import { scheduleArray } from 'rxjs/internal/scheduled/scheduleArray';
import { FriendRequestModule } from './friend-request.module';

@Module({
  imports: [
    MongooseModule.forFeature([
      {name: User.name, schema: UserSchema},
      {name: FriendRequest.name, schema: FriendRequestSchema},
    ]),
    FriendRequestModule
  ],
  controllers: [UserController],
  providers: [UserService],
  exports: [UserService],
})
export class UserModule {}
