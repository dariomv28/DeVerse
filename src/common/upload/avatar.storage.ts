import { diskStorage } from 'multer'
import { extname } from 'path'
import { Request } from 'express'

export const avatarStorage = diskStorage({
    destination: (req, file, cb) => {
        cb(null, './uploads/avatars')
    },
    filename: (req: Request & { user: any }, file, cb) => {
        const userId = req.user.userId
        const ext = extname(file.originalname)
        const filename = `${userId}${ext}`
        cb(null, filename)
    },
})