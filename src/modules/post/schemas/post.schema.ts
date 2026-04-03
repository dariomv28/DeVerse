import { Prop, Schema, SchemaFactory } from "@nestjs/mongoose";
import { Document, Types } from "mongoose";

export type PostDocument = Post & Document
@Schema({ timestamps: true })
export class Post {
    @Prop({ type: Types.ObjectId, ref: 'User', required: true })
    author!: Types.ObjectId;

    @Prop()
    content!: string;

    @Prop([String])
    images!: string[];

    @Prop({ default: 0 })
    likeCount!: number;

    @Prop({ default: 0 })
    commentCount!: number;
}

export const PostSchema = SchemaFactory.createForClass(Post);