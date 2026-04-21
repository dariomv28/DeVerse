import { Module } from '@nestjs/common';
import { MongooseModule } from '@nestjs/mongoose';
import { Post, PostSchema } from './schemas/post.schema';
import { PostService } from './post.service';
import { PostController } from './post.controller';
import { Comment, CommentSchema } from './schemas/comment.schema';
import { Like, LikeSchema } from './schemas/like.schema';
import { PostGateway } from './post.gateway';

@Module({
  imports: [
    MongooseModule.forFeature([
      { name: Post.name, schema: PostSchema },
      { name: Comment.name, schema: CommentSchema},
      { name: Like.name, schema: LikeSchema}
    ]),
  ],
  controllers: [PostController],
  providers: [PostService, PostGateway],
  exports: [PostService],
})
export class PostModule {}