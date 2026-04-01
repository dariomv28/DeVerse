import { Controller, Get, Post, Param, Query, Req, UseGuards } from '@nestjs/common'
import { UserService } from './user.service'
import { AuthGuard } from '@nestjs/passport'
import { FriendRequestService } from './friend-request.service'

@Controller('users')
export class UserController {
  constructor(
    private readonly userService: UserService,
    private readonly friendRequestService: FriendRequestService,
  ) {}

  @Get('search')
  @UseGuards(AuthGuard('jwt'))
  search(@Query('q') q: string) {
    return this.userService.search(q)
  }

  @Post(':id/follow')
  @UseGuards(AuthGuard('jwt'))
  follow(@Param('id') targetId: string, @Req() req) {
    return this.userService.follow(req.user.userId, targetId)
  }

  @Post(':id/add-friend')
  @UseGuards(AuthGuard('jwt'))
  addFriend(@Param('id') targetId: string, @Req() req) {
    return this.friendRequestService.createRequest(
      req.user.userId,
      targetId,
    )
  }

  // GET REQUESTS
  @Get('requests')
  @UseGuards(AuthGuard('jwt'))
  getRequests(@Req() req) {
    return this.friendRequestService.getRequests(req.user.userId)
  }

  // ACCEPT
  @Post('request/:id/accept')
  @UseGuards(AuthGuard('jwt'))
  accept(@Param('id') id: string, @Req() req) {
    return this.friendRequestService.acceptRequest(
      id,
      req.user.userId,
    )
  }

  // REJECT
  @Post('request/:id/reject')
  @UseGuards(AuthGuard('jwt'))
  reject(@Param('id') id: string, @Req() req) {
    return this.friendRequestService.rejectRequest(
      id,
      req.user.userId,
    )
  }
}