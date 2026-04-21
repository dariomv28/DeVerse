import * as path from 'path'
import { diskStorage } from 'multer'
import { extname } from 'path'
import * as fs from 'fs';

const uploadRoot = path.join(process.cwd(), 'uploads')

export const postStorage = diskStorage({
    destination: (req, file, cb) => {
        const temp = path.join(uploadRoot, 'temp')

        if (!fs.existsSync(temp)) {
            fs.mkdirSync(temp, { recursive: true })
        }

        cb(null, temp)
    },
    filename: (req, file, cb) => {
        const ext = extname(file.originalname)
        cb(null, Date.now() + ext)
    },
})