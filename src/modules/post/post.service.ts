import { Injectable, NotFoundException, ForbiddenException } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { Model } from 'mongoose';
import { Post, PostDocument } from './schemas/post.schema';
import { Comment, CommentDocument } from './schemas/comment.schema';
import { Like, LikeDocument } from './schemas/like.schema';
import * as fs from 'fs';

@Injectable()
export class PostService {
    constructor(
        @InjectModel(Post.name) private postModel: Model<PostDocument>,
        @InjectModel(Comment.name) private commentModel: Model<CommentDocument>,
        @InjectModel(Like.name) private likeModel: Model<LikeDocument>
    ) {}

    async create(content: string, files: any[], userId: string) {
        const session = await this.postModel.db.startSession();
        session.startTransaction();

        let dir = '';

        try {
            const [post] = await this.postModel.create(
                [
                    {
                        content,
                        images: [],
                        author: userId,
                    },
                ],
                { session }
            );

            dir = `./uploads/posts/${post._id}`;
            if (!fs.existsSync(dir)) {
                fs.mkdirSync(dir, { recursive: true });
            }

            const imagePaths: string[] = [];

            for (const file of files) {
                const newPath = `${dir}/${file.filename}`;

                fs.renameSync(file.path, newPath);

                imagePaths.push(`/uploads/posts/${post._id}/${file.filename}`);
            }

            await this.postModel.updateOne(
                { _id: post._id },
                { images: imagePaths }
            ).session(session);

            await session.commitTransaction();
            session.endSession();

            return {
                ...post.toObject(),
                images: imagePaths,
            };
        } catch (error) {
            await session.abortTransaction();
            session.endSession();

            // cleanup file temp
            for (const file of files) {
                if (fs.existsSync(file.path)) {
                    fs.unlinkSync(file.path);
                }
            }

            // cleanup folder nếu đã tạo
            if (dir && fs.existsSync(dir)) {
                fs.rmSync(dir, { recursive: true, force: true });
            }

            throw error;
        }
    }

    async getFeed() {
        const posts = await this.postModel
            .find()
            .populate('author', 'name')
            .sort({ createdAt: -1 })
            .lean();

        const postIds = posts.map(p => p._id);

        const comments = await this.commentModel
            .find({ post: { $in: postIds } })
            .populate('author', 'name')
            .sort({ createdAt: -1 })
            .lean();

        const likes = await this.likeModel
            .find({ pid: { $in: postIds } })
            .populate('uid', 'name')
            .sort({ createdAt: -1 })
            .lean();

        return posts.map(post => ({
            ...post,
            comments: comments.filter(c => c.post.toString() === post._id.toString()),
            likes: likes.filter(l => l.pid.toString() === post._id.toString()),
        }));
    }

    async getById(postId: string) {
        const post = await this.postModel
            .findById(postId)
            .populate('author', 'name')
            .lean();

        if (!post) {
            throw new NotFoundException('Post not found');
        }

        const comments = await this.commentModel
            .find({ post: post._id })
            .populate('author', 'name')
            .sort({ createdAt: -1 })
            .lean();

        const likes = await this.likeModel
            .find({ pid: post._id })
            .populate('uid', 'name')
            .sort({ createdAt: -1 })
            .lean();

        return {
            ...post,
            comments,
            likes
        };
    }

    async deletePost(postId: string, userId: string) {
        const session = await this.postModel.db.startSession();
        session.startTransaction();

        try {
            const post = await this.postModel.findById(postId).session(session);

            if (!post) throw new NotFoundException('Post not found');

            if (post.author.toString() !== userId) {
                throw new ForbiddenException('Only authors can delete their posts');
            }

            await this.commentModel.deleteMany({ post: post._id }).session(session);
            await this.likeModel.deleteMany({ pid: post._id }).session(session);
            await this.postModel.deleteOne({ _id: post._id }).session(session);

            await session.commitTransaction();
            session.endSession();

            return { message: 'Post and related data deleted' };
        } catch (error) {
            await session.abortTransaction();
            session.endSession();
            throw error;
        }
    }

    async toggleLike(postId: string, userId: string) {
        const session = await this.postModel.db.startSession();
        session.startTransaction();

        try {
            const post = await this.postModel.findById(postId).session(session);
            if (!post) throw new NotFoundException('Post not found');

            const existingLike = await this.likeModel.findOne({
                pid: postId,
                uid: userId,
            }).session(session);

            if (existingLike) {
                await this.likeModel.deleteOne({ _id: existingLike._id }).session(session);
                await this.postModel.updateOne(
                    { _id: postId },
                    { $inc: { likeCount: -1 } }
                ).session(session);

                await session.commitTransaction();
                session.endSession();
                return { liked: false };
            }

            await this.likeModel.create(
                [{ pid: postId, uid: userId }],
                { session }
            );

            await this.postModel.updateOne(
                { _id: postId },
                { $inc: { likeCount: 1 } }
            ).session(session);

            await session.commitTransaction();
            session.endSession();
            return { liked: true };
        } catch (error) {
            await session.abortTransaction();
            session.endSession();
            throw error;
        }
    }

    async postComment(postId: string, userId: string, content: string) {
        const session = await this.postModel.db.startSession();
        session.startTransaction();

        try {
            const post = await this.postModel.findById(postId).session(session);
            if (!post) throw new NotFoundException('Post not found');

            const [comment] = await this.commentModel.create(
                [{ post: postId, author: userId, content }],
                { session }
            );

            await this.postModel.updateOne(
                { _id: postId },
                { $inc: { commentCount: 1 } }
            ).session(session);

            await session.commitTransaction();
            session.endSession();

            return comment;
        } catch (error) {
            await session.abortTransaction();
            session.endSession();
            throw error;
        }
    }

    async deleteComment(commentId: string, userId: string) {
        const session = await this.postModel.db.startSession();
        session.startTransaction();

        try {
            const comment = await this.commentModel.findById(commentId).session(session);
            if (!comment) throw new NotFoundException('Comment not found');

            const post = await this.postModel.findById(comment.post).session(session);
            if (!post) throw new NotFoundException('Post not found');

            if (
                comment.author.toString() !== userId &&
                post.author.toString() !== userId
            ) {
                throw new ForbiddenException('You cannot delete this comment');
            }

            await this.commentModel.deleteOne({ _id: commentId }).session(session);

            await this.postModel.updateOne(
                { _id: comment.post },
                { $inc: { commentCount: -1 } }
            ).session(session);

            await session.commitTransaction();
            session.endSession();

            return { message: 'Comment deleted' };
        } catch (error) {
            await session.abortTransaction();
            session.endSession();
            throw error;
        }
    }
}