import { Prop, Schema, SchemaFactory } from "@nestjs/mongoose";
import { Document, Types } from "mongoose";

export type LikeDocument = Like & Document
@Schema({timestamps: true})
export class Like {
    @Prop({type: Types.ObjectId, ref: 'User', required: true})
    uid!: Types.ObjectId;

    @Prop({type: Types.ObjectId, ref: 'Post', required: true})
    pid!: Types.ObjectId
}

export const LikeSchema = SchemaFactory.createForClass(Like);