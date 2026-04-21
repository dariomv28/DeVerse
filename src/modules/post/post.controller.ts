import {
    Controller,
    Post as HttpPost,
    Get,
    Delete,
    Param,
    Body,
    Req,
    UseGuards,
    UploadedFiles,
    Query,
    UseInterceptors,
} from '@nestjs/common';
import { PostService } from './post.service';
import { AuthGuard } from '@nestjs/passport';
import { postStorage } from '../../common/upload/postStorage';
import { FilesInterceptor } from '@nestjs/platform-express';

@Controller('posts')
export class PostController {
    constructor(private readonly postService: PostService) {}

    // ================= CREATE POST =================
    @UseGuards(AuthGuard('jwt'))
    @HttpPost()
    @UseInterceptors(
        FilesInterceptor('image', 10, {
            storage: postStorage,
        }),
    )
    createPost(
        @UploadedFiles() files: Express.Multer.File[] = [],
        @Body('content') content: string,
        @Req() req,
    ) {
        return this.postService.create(
            content?.trim() || '',
            files,
            req.user.userId,
        );
    }

    // ================= FEED =================
    @UseGuards(AuthGuard('jwt'))
    @Get('feed')
    getFeed(
        @Query('page') page: string,
        @Query('limit') limit: string,
        @Req() req,
    ) {
        return this.postService.getFeed(
            Number(page) || 1,
            Number(limit) || 5,
            req.user?.userId,
        );
    }

    // ================= GET BY ID =================
    @UseGuards(AuthGuard('jwt'))
    @Get(':id')
    getPost(@Param('id') id: string, @Req() req) {
        return this.postService.getById(id, req.user.userId);
    }

    // ================= DELETE POST =================
    @UseGuards(AuthGuard('jwt'))
    @Delete(':id')
    deletePost(@Param('id') id: string, @Req() req) {
        return this.postService.deletePost(id, req.user.userId);
    }

    // ================= LIKE =================
    @UseGuards(AuthGuard('jwt'))
    @HttpPost(':id/like')
    toggleLike(@Param('id') id: string, @Req() req) {
        return this.postService.toggleLike(id, req.user.userId);
    }

    // ================= COMMENT =================
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
            content?.trim() || '',
        );
    }

    // ================= DELETE COMMENT =================
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