import { Injectable, NotFoundException, ForbiddenException } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { Model } from 'mongoose';
import { Post, PostDocument } from './schemas/post.schema';

@Injectable()
export class PostService {
    constructor(
        @InjectModel(Post.name)
        private postModel: Model<PostDocument>,
    ) { }

    async create(content: string, images: string[], userId: string) {
        const post = await this.postModel.create({
            content,
            images,
            author: userId,
        });
        return post;
    }

    async getFeed() {
        return this.postModel
            .find()
            .populate('author', 'name')
            .sort({ createdAt: -1 });
    }

    async getById(postId: string) {
        const post = await this.postModel
            .findById(postId)
            .populate('author', 'name');

        if (!post) throw new NotFoundException('Post not found');
        return post;
    }

    async delete(postId: string, userId: string) {
        const post = await this.postModel.findById(postId);

        if (!post) throw new NotFoundException('Post not found');

        if (post.author.toString() !== userId) {
            throw new ForbiddenException('You cannot delete this post');
        }

        await post.deleteOne();

        return { message: 'Post deleted' };
    }

    async like(postId: string) {
        const post = await this.postModel.findByIdAndUpdate(
            postId,
            { $inc: { likeCount: 1 } },
            { new: true },
        );

        if (!post) throw new NotFoundException('Post not found');

        return post;
    }
}