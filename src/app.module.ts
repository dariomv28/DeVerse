import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { MongooseModule } from '@nestjs/mongoose';
import Redis from 'ioredis';
import { RedisModule } from './redis/redis.module';
import { TestModule } from './test/test.module';
import { UserModule } from './modules/user/user.module';
import { AuthModule } from './modules/auth/auth.module';
import { TaskModule } from './modules/task/task.module';
import {ConfigModule, ConfigService } from '@nestjs/config';
import { PostModule } from './modules/post/post.module';

@Module({
  imports: 
  [
    ConfigModule.forRoot({isGlobal: true,}),
    MongooseModule.forRootAsync({
      inject: [ConfigService],
      useFactory: (config: ConfigService) => ({
        uri: config.get<string>('MONGO_URI'),
      }),
    }),
    RedisModule.forRootAsync(),
    TestModule, 
    UserModule, 
    AuthModule, 
    TaskModule, PostModule,
  ],
  controllers: [AppController],
  providers: [AppService],
})
export class AppModule {}