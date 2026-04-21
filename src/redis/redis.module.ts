import { Global, Module } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import Redis from 'ioredis';

@Global()
@Module({})
export class RedisModule {
  static forRootAsync() {
    return {
      module: RedisModule,
      providers: [
        {
          provide: 'REDIS_CLIENT',
          inject: [ConfigService],
          useFactory: async (config: ConfigService) => {
            const client = new Redis({
              host: config.get<string>('REDIS_HOST'),
              port: config.get<number>('REDIS_PORT'),
            });

            // test connection
            client.on('connect', () => {
              console.log('✅ Redis connected');
            });

            client.on('error', (err) => {
              console.error('❌ Redis error:', err);
            });

            return client;
          },
        },
      ],
      exports: ['REDIS_CLIENT'],
    };
  }
}