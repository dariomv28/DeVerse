import { WebSocketGateway, WebSocketServer } from '@nestjs/websockets'
import { Server } from 'socket.io'

@WebSocketGateway({
  cors: {
    origin: '*',
  },
})
export class UserGateway {
  @WebSocketServer()
  server!: Server

  emitFriendRequest(payload: any) {
    this.server.emit('friend.request', payload)
  }

  emitFriendRequestCancelled(payload: any) {
    this.server.emit('friend.request.cancelled', payload)
  }

  emitFriendAccepted(payload: any) {
    this.server.emit('friend.accepted', payload)
  }

  emitFollow(payload: any) {
    this.server.emit('user.followed', payload)
  }
}
