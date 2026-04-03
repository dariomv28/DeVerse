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
        return this.postService.delete(id, req.user.userId);
    }

    @UseGuards(AuthGuard('jwt'))
    @HttpPost(':id/like')
    likePost(@Param('id') id: string) {
        return this.postService.like(id);
    }
}