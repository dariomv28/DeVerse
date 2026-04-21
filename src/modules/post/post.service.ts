import {
    Inject,
    Injectable,
    NotFoundException,
    ForbiddenException,
} from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import Redis from 'ioredis';
import { Model } from 'mongoose';
import { Post, PostDocument } from './schemas/post.schema';
import { Comment, CommentDocument } from './schemas/comment.schema';
import { Like, LikeDocument } from './schemas/like.schema';
import * as path from 'path';
import * as fs from 'fs';
import { PostGateway } from './post.gateway';

@Injectable()
export class PostService {
    constructor(
        @Inject('REDIS_CLIENT') private readonly redis: Redis,
        private readonly postGateway: PostGateway,
        @InjectModel(Post.name) private postModel: Model<PostDocument>,
        @InjectModel(Comment.name) private commentModel: Model<CommentDocument>,
        @InjectModel(Like.name) private likeModel: Model<LikeDocument>,
    ) {}

    async create(content: string, files: any[], userId: string) {
        let dir = '';
        const uploadRoot = path.join(process.cwd(), 'uploads');

        try {
            const post = await this.postModel.create({
                content,
                images: [],
                author: userId,
            });

            dir = path.join(uploadRoot, 'posts', post._id.toString());
            if (!fs.existsSync(dir)) {
                fs.mkdirSync(dir, { recursive: true });
            }

            const imagePaths: string[] = [];

            for (const file of files) {
                const newPath = path.join(dir, file.filename);
                fs.copyFileSync(file.path, newPath);
                fs.unlinkSync(file.path);

                imagePaths.push(`/uploads/posts/${post._id}/${file.filename}`);
            }

            post.images = imagePaths;
            await post.save();

            const populatedPost = await this.postModel
                .findById(post._id)
                .populate('author', 'name avatar')
                .lean();

            this.postGateway.server.emit('post.created', {
                post: populatedPost,
            });

            await this.clearAllFeedCache();

            return populatedPost;
        } catch (error) {
            for (const file of files) {
                if (fs.existsSync(file.path)) fs.unlinkSync(file.path);
            }

            if (dir && fs.existsSync(dir)) {
                fs.rmSync(dir, { recursive: true, force: true });
            }

            throw error;
        }
    }

    async getFeed(page = 1, limit = 5, userId?: string) {
        const cacheKey = `feed:${page}:${limit}:${userId || 'guest'}`;
        const cached = await this.redis.get(cacheKey);
        if (cached) return JSON.parse(cached);

        const skip = (page - 1) * limit;

        const posts = await this.postModel
            .find()
            .populate('author', 'name avatar')
            .sort({ createdAt: -1 })
            .skip(skip)
            .limit(limit)
            .lean();

        const postIds = posts.map(p => p._id);

        const comments = await this.commentModel
            .find({ post: { $in: postIds } })
            .populate('author', 'name avatar')
            .sort({ createdAt: -1 })
            .lean();

        const likeCounts = await this.likeModel.aggregate([
            { $match: { pid: { $in: postIds } } },
            { $group: { _id: '$pid', count: { $sum: 1 } } }
        ]);

        const likeMap = new Map(
            likeCounts.map(l => [l._id.toString(), l.count])
        );

        let likedSet = new Set<string>();

        if (userId) {
            const userLikes = await this.likeModel.find({
                pid: { $in: postIds },
                uid: userId,
            }).lean();

            likedSet = new Set(userLikes.map(l => l.pid.toString()));
        }

        const result = posts.map(post => {
            const postComments = comments.filter(
                c => c.post.toString() === post._id.toString()
            );

            return {
                ...post,
                comments: postComments,
                likeCount: likeMap.get(post._id.toString()) || 0,
                commentCount: postComments.length,
                isLiked: likedSet.has(post._id.toString()),
            };
        });

        await this.redis.set(cacheKey, JSON.stringify(result), 'EX', 10);

        return result;
    }

    async getById(postId: string, userId?: string) {
        const post = await this.postModel
            .findById(postId)
            .populate('author', 'name avatar')
            .lean();

        if (!post) throw new NotFoundException('Post not found');

        const comments = await this.commentModel
            .find({ post: post._id })
            .populate('author', 'name avatar')
            .sort({ createdAt: -1 })
            .lean();

        const likeCount = await this.likeModel.countDocuments({
            pid: post._id,
        });

        let isLiked = false;

        if (userId) {
            isLiked = !!(await this.likeModel.findOne({
                pid: postId,
                uid: userId,
            }));
        }

        return {
            ...post,
            comments,
            likeCount,
            commentCount: comments.length,
            isLiked,
        };
    }

    async deletePost(postId: string, userId: string) {
        const post = await this.postModel.findById(postId);

        if (!post) throw new NotFoundException('Post not found');

        if (post.author.toString() !== userId) {
            throw new ForbiddenException('Only authors can delete');
        }

        await this.commentModel.deleteMany({ post: postId });
        await this.likeModel.deleteMany({ pid: postId });
        await this.postModel.deleteOne({ _id: postId });

        const dir = path.join(
            process.cwd(),
            'uploads/posts',
            postId.toString()
        );

        if (fs.existsSync(dir)) {
            fs.rmSync(dir, { recursive: true, force: true });
        }

        this.postGateway.server.emit('post.deleted', {
            postId,
        });

        await this.clearAllFeedCache();

        return { message: 'Deleted' };
    }

    async toggleLike(postId: string, userId: string) {
        const existing = await this.likeModel.findOne({
            pid: postId,
            uid: userId,
        });

        let isLiked = false;

        if (existing) {
            await this.likeModel.deleteOne({ _id: existing._id });
            isLiked = false;
        } else {
            await this.likeModel.create({
                pid: postId,
                uid: userId,
            });
            isLiked = true;
        }

        const likeCount = await this.likeModel.countDocuments({
            pid: postId,
        });

        this.postGateway.server.emit('post.updated', {
            postId,
            likeCount,
            isLiked,
            userId,
            type: 'LIKE_UPDATE',
        });

        await this.clearAllFeedCache();

        return { likeCount, isLiked };
    }

    async postComment(postId: string, userId: string, content: string) {
        const comment = await this.commentModel.create({
            post: postId,
            author: userId,
            content,
        });

        const populatedComment = await this.commentModel
            .findById(comment._id)
            .populate('author', 'name avatar')
            .lean();

        const commentCount = await this.commentModel.countDocuments({
            post: postId,
        });

        this.postGateway.server.emit('post.updated', {
            postId,
            commentCount,
            comment: populatedComment,
            type: 'COMMENT_ADD',
        });

        await this.clearAllFeedCache();

        return populatedComment;
    }

    async deleteComment(commentId: string, userId: string) {
        const comment = await this.commentModel.findById(commentId);

        if (!comment) throw new NotFoundException();

        await this.commentModel.deleteOne({ _id: commentId });

        const commentCount = await this.commentModel.countDocuments({
            post: comment.post,
        });

        this.postGateway.server.emit('post.updated', {
            postId: comment.post,
            commentCount,
            commentId: commentId,
            type: 'COMMENT_DELETE',
        });

        await this.clearAllFeedCache();

        return { message: 'Deleted' };
    }

    private async clearAllFeedCache() {
        const keys = await this.redis.keys('feed:*');
        if (keys.length) {
            await this.redis.del(...keys);
        }
    }
}