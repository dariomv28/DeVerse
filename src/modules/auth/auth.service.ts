import { Injectable, BadRequestException } from '@nestjs/common';
import { UserService } from '../user/user.service';
import * as bcrypt from 'bcrypt';
import { JwtService } from '@nestjs/jwt';
import { ConfigService } from '@nestjs/config'
import { access } from 'fs';
import * as nodemailer from 'nodemailer'

@Injectable()
export class AuthService {
    constructor(
        private userService: UserService,
        private jwtService: JwtService,
        private configService: ConfigService,
    ) {}
    async register(email: string, password: string, name: string) {
        const existingEmail = await this.userService.findByEmail(email);
        if (existingEmail) {
            throw new BadRequestException('Email already exists');
        }
        const hashed_password = await bcrypt.hash(password, 10);
        const user = await this.userService.create({
            email,
            password: hashed_password,
            name,
        });
        return user;
    }
    async login(email:string, password:string) {
        const existingUser = await this.userService.findByEmail(email);
        if (!existingUser) {
            throw new BadRequestException("Email does not exist");
        }
        const match = await bcrypt.compare(password, existingUser.password);
        if (!match) {
            throw new BadRequestException("Wrong password");
        }
        const token = this.jwtService.sign({
            userId: existingUser._id,
        });
        return {
            access_token: token
        }
    }
    async changePassword(email:string, oldPassword:string, newPassword:string) {
        const currentUser = await this.userService.findByEmail(email);
        if (!currentUser) {
            throw new BadRequestException("Email does not exist");
        }
        const match = await bcrypt.compare(oldPassword, currentUser.password);
        if (!match) {
            throw new BadRequestException("Wrong password");
        }
        const hashed_password = await bcrypt.hash(newPassword, 10);
        await this.userService.changePassword(email, hashed_password)
        const token = this.jwtService.sign({
            userId: currentUser._id,
        });
        return {
            access_token: token
        }
    }
    async sendPasswordRecoveryEmail(email:string) {
        const user = await this.userService.findByEmail(email);
        if (!user) {
            throw new BadRequestException('Email does not exist');
        }
        const token = this.jwtService.sign(
            {email},
            {expiresIn: '10m'}
        );
        const resetLink = `http://localhost:3001/auth/reset-password?token=${token}`;
        const transporter = nodemailer.createTransport({
            service: 'gmail',
            auth: {
                user: this.configService.get<string>('MAIL_USER'),
                pass: this.configService.get<string>('MAIL_PASS'),
            },
        });
        await transporter.sendMail({
            to: email,
            subject: 'Reset Password',
            html: `
                <h3>Reset your password</h3>
                <a href="${resetLink}">Click here to reset</a>
            `,
        });
        return {message: 'Reset email sent'}
    }
    async resetPassword(token:string, newPassword: string) {
        try {
            const payload = this.jwtService.verify(token);
            const hashed_password = await bcrypt.hash(newPassword, 10);
            await this.userService.changePassword(payload.email, hashed_password);
            return {message: 'Password reset successful'}
        } catch(err) {
            throw new BadRequestException('Ivalid or expired token');
        }
    }
}