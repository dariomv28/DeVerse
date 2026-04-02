import { diskStorage } from 'multer'
import { extname } from 'path'
import { Request } from 'express'
import * as fs from 'fs'

export const avatarStorage = diskStorage({
    destination: (req, file, cb) => {
        const uploadPath = './uploads/avatars'
        if (!fs.existsSync(uploadPath)) {
            fs.mkdirSync(uploadPath, { recursive: true })
        }
        cb(null, uploadPath)
    },

    filename: (req: Request & { user: any }, file, cb) => {
        const userId = req.user.userId
        const ext = extname(file.originalname)
        const filename = `${userId}${ext}`
        cb(null, filename)
    },
})