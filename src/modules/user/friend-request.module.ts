import { Module } from "@nestjs/common";
import { MongooseModule } from "@nestjs/mongoose";
import { FriendRequest } from "./schemas/friend_request.schema";
import { FriendRequestSchema } from "./schemas/friend_request.schema";
import { FriendRequestService } from "./friend-request.service";
import { User } from "./schemas/user.schema";
import { UserSchema } from "./schemas/user.schema";
import { UserGateway } from './user.gateway';

@Module({
  imports: [
    MongooseModule.forFeature([
      { name: FriendRequest.name, schema: FriendRequestSchema },
      { name: User.name, schema: UserSchema },
    ]),
  ],
  providers: [FriendRequestService, UserGateway],
  exports: [FriendRequestService, UserGateway],
})
export class FriendRequestModule {}