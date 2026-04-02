import { Prop, Schema, SchemaFactory } from "@nestjs/mongoose";
import { Document, Types } from "mongoose";
import { ref } from "process";
export type UserDocument = User & Document;
@Schema()
export class User {

    @Prop({unique : true})
    email!: string;

    @Prop()
    password!: string;

    @Prop()
    name!: string;

    @Prop({type : [{type: Types.ObjectId, ref : 'User'}]})
    followers!: Types.ObjectId[];

    @Prop({type : [{type : Types.ObjectId, ref : 'User'}]})
    following!: Types.ObjectId[];

    @Prop({type : [{type : Types.ObjectId, ref : 'User'}]})
    friends!: Types.ObjectId[];

    @Prop({ default: '' })
    avatar!: string;
}
export const UserSchema = SchemaFactory.createForClass(User);