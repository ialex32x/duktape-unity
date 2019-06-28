const gulp = require('gulp')
const fs = require('fs')
const path = require('path')
const concat = require('gulp-concat');
const uglifyes = require('uglify-es');
const composer = require('gulp-uglify/composer');
const minify = composer(uglifyes, console);
const babel = require('gulp-babel');
const clean = require('gulp-clean');
const sequence = require('run-sequence');
// const oss = require('ali-oss')
const crypto = require('crypto')

const self_config = require('./gulpfile-config')

function walk_generic_files(basepath, innerpath, filter, callback) {
    let filepath = path.join(basepath, innerpath)
    let items = fs.readdirSync(filepath)
    for (let i = 0; i < items.length; i++) {
        let item = items[i]
        let itempath = path.join(filepath, item)
        let inneritempath = path.join(innerpath, item)
        let stat = fs.statSync(itempath)
        if (stat.isDirectory()) {
            walk_generic_files(basepath, inneritempath, filter, callback)
        } else if (stat.isFile()) {
            if (filter(itempath)) {
                callback(inneritempath, itempath)
            }
        }
    }
}

// 上传阿里云
function updateCloudRes() {
    // let client = new oss(self_config.oss_conf)
    // function updateRemoteFiles() {
    //     // 上传并检查变更
    //     walk_generic_files(self_config.oss_path.local, '', function () {
    //         return true
    //     }, function (inneritempath, itempath) {
    //         async function updatingRemoteFiles() {
    //             let destpath = `${self_config.oss_path.remote}/${inneritempath}`.replace('\\', '/')
    //             try {
    //                 let content = fs.readFileSync(itempath)
    //                 let localhash = crypto.createHash('md5').update(content).digest('hex')
    //                 let remotehash = ''
    //                 let result = await client.list({
    //                     prefix: destpath
    //                 })
    //                 let skip = false
    //                 if (!!result.objects && result.objects.length > 0) {
    //                     let head = await client.head(destpath)
    //                     if (!!head.meta) {
    //                         remotehash = head.meta.hash
    //                         if (remotehash == localhash) {
    //                             skip = true
    //                         }
    //                     }
    //                 }
    //                 if (!skip) {
    //                     await client.put(destpath, itempath, {
    //                         meta: {
    //                             hash: localhash,
    //                             tool: 'gulp'
    //                         }
    //                     })
    //                     console.log("upload: ", itempath, `[${localhash}]`, " => ", destpath, `[${remotehash}]`)
    //                 } else {
    //                     console.log("skip: ", itempath, `[${localhash}]`)
    //                 }
    //             } catch (e) {
    //             }
    //         }
    //         updatingRemoteFiles().catch(reason => {
    //             console.error("updatingRemoteFiles failed:", reason.name)
    //         })
    //     })
    // }

    // // 遍历云端文件列表，检查本地删除项
    // async function checkingRemoteFiles() {
    //     let remoteFiles = await client.list({
    //         prefix: `${self_config.oss_path.remote}/`
    //     })
    //     if (!!remoteFiles.objects && remoteFiles.objects.length > 0) {
    //         let deletefiles = []
    //         remoteFiles.objects.forEach(function (obj) {
    //             let localPath = `${self_config.oss_path.local}${obj.name.substring(self_config.oss_path.remote.length)}`
    //             if (!fs.existsSync(localPath)) {
    //                 deletefiles.push(obj.name)
    //                 console.log("delete remote file", obj.name)
    //             }
    //         })
    //         let result = await client.deleteMulti(deletefiles)
    //         console.log("aliyun oss delete result:", result)
    //     }
    //     updateRemoteFiles()
    // }
    // checkingRemoteFiles().catch(reason => {
    //     console.error("checkingRemoteFiles failed:", reason.name)
    //     updateRemoteFiles()
    // })
    // // client.put('')
}

// 合并用户JS
gulp.task('compile', function () {
    let stream = gulp.src(self_config.source)
        // .pipe(babel())
        // .pipe(concat('compiled.js'))
        .pipe(minify())
        .pipe(gulp.dest(self_config.releasepath));
    return stream;
});

// 清空发布目录
gulp.task('clean', function () {
    let stream = gulp.src(self_config.releasepath + '*')
        .pipe(clean({ force: true }))
    return stream;
});

gulp.task('cloud', function () {
    updateCloudRes()
})

// 打包 
gulp.task('release', function (cb) {
    target_platform = "release"
    sequence('clean', 'compile', cb)
});
