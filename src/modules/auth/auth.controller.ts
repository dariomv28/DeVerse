import { Body, Controller, Post, Get, Req, UseGuards } from '@nestjs/common';
import { AuthService } from './auth.service';
import { AuthGuard } from '@nestjs/passport';

@Controller('auth')
export class AuthController {
    constructor(private authService: AuthService) {}
    @Post('register')
    register(
        @Body('email') email: string,
        @Body('password') password: string,
        @Body('name') name: string,
    ) {
        return this.authService.register(email, password, name);
    }
    @Post('login')
    login(
        @Body('email') email: string,
        @Body('password') password: string,
    ){
        return this.authService.login(email, password);
    }
    @Get('me')
    @UseGuards(AuthGuard('jwt'))
    getMe(@Req() req){
        return req.user;
    }
    @Post('change-password')
    @UseGuards(AuthGuard('jwt'))
    changePassword(
        @Body('email') email: string,
        @Body('oldPassword') oldPassword: string,
        @Body('newPassword') newPassword: string,
    ) {
        return this.authService.changePassword(email, oldPassword, newPassword)
    }
}
