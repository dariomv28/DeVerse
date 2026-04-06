import {
    Controller,
    Post as HttpPost,
    Get,
    Delete,
    Param,
    Body,
    Req,
    UseGuards,
} from '@nestjs/common';
import { PostService } from './post.service';
import { AuthGuard } from '@nestjs/passport';

@Controller('posts')
export class PostController {
    constructor(private readonly postService: PostService) { }

    @UseGuards(AuthGuard('jwt'))
    @HttpPost()
    createPost(
        @Body() body: { content: string; images?: string[] },
        @Req() req,
    ) {
        return this.postService.create(
            body.content,
            body.images || [],
            req.user.userId,
        );
    }

    @Get('feed')
    getFeed() {
        return this.postService.getFeed();
    }

    @Get(':id')
    getPost(@Param('id') id: string) {
        return this.postService.getById(id);
    }

    @UseGuards(AuthGuard('jwt'))
    @Delete(':id')
    deletePost(@Param('id') id: string, @Req() req) {
        return this.postService.deletePost(id, req.user.userId);
    }

    @UseGuards(AuthGuard('jwt'))
    @HttpPost(':id/like')
    toggleLike(@Param('id') id: string, @Req() req) {
        return this.postService.toggleLike(id, req.user.userId);
    }

    @UseGuards(AuthGuard('jwt'))
    @HttpPost(':id/comment')
    postComment(
        @Param('id') postId: string,
        @Body('content') content: string,
        @Req() req,
    ) {
        return this.postService.postComment(
            postId,
            req.user.userId,
            content,
        );
    }

    @UseGuards(AuthGuard('jwt'))
    @Delete('comment/:commentId')
    deleteComment(
        @Param('commentId') commentId: string,
        @Req() req,
    ) {
        return this.postService.deleteComment(
            commentId,
            req.user.userId,
        );
    }
}