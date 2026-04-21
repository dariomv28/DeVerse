import {
    WebSocketGateway,
    WebSocketServer,
    SubscribeMessage,
    OnGatewayConnection,
    OnGatewayDisconnect,
} from '@nestjs/websockets';

import { Server, Socket } from 'socket.io';

@WebSocketGateway({
    cors: {
        origin: '*',
    },
})
export class PostGateway implements OnGatewayConnection, OnGatewayDisconnect {

    @WebSocketServer()
    server!: Server;

    handleConnection(client: Socket) {
        console.log('Client connected:', client.id);
    }

    handleDisconnect(client: Socket) {
        console.log('Client disconnected:', client.id);
    }

    // helper emit
    emitPostUpdated(payload: any) {
        this.server.emit('post.updated', payload);
    }

    emitPostDeleted(postId: string) {
        this.server.emit('post.deleted', { postId });
    }
}