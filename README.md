# S3Tools

## s3-cli 工具

> 解决在windows,linux 环境下测试相关的存储是否正常运行

## s3-cli 支持功能

- 配置`s3-cli`的相关参数
- 列出 `bucket`
- 列出指定数量的文件
- 上传指定的文件或指定目录下的文件/生成指定大小的文件上传到s3
- 下载指定的一个或多个文件
- 删除一个或多个文件
- `s3`文件拷贝
- `bucket`权限查询
- 生成文件的访问路径

## Windows下添加`s3-cli`工具

- 将当前程序目录添加到Windows的环境变量的`PATH` 路径下,随后就可以使用 `s3-cli` 命令进行相关的操作

## Linux下添加`s3-cli`工具

- 将程序拷贝到Linux相关路径下,如目前程序的目录地址为: `/data/S3Cli_Linux`
- 授权
  - `sudo chmod +x s3-cli`
- 添加程序目录
  - `vi /etc/profile`
  - 在`export`下新增如下配置 `export PATH=$PATH:/data/S3Cli_Linux`
  - `source /etc/profile`

## 相关命令

> 使用 `s3-cli -h`可以获取相关的命令参数,在各级的命令下可以使用`-h`来获取当前命令的参数,如: `s3-cli config -h`,`s3-cli config set -h`

### config命令

#### config info命令

- 获取参数配置信息`s3-cli config info`

#### config set命令

> `config set`为参数配置命令

- `-v|--vendor`,s3的版本参数,由于金山云的KS3与其他的sdk不通用,因此 `-v`参数可以设置为`Amazon`或者 `KS3`(金山云KS3)
- `-ak|--accesskey_id` AccessKeyId
- `-sk|--secret_accesskey` SecretAccessKey
- `-s|--server_url` 服务地址
- `-b|--bucket` 默认的bucket名
- `-f|--force_path` 该参数参考`Amazon SDK`中的`ForcePathStyle`
- `-sv|--sign_version`签名版本,`KS3`访问时应使用 `2`或`2.0`
- `-t|--tempoary` 文件上传下载等临时存储文件目录

>例如: `s3-cli config set -v:Amazon -ak:minioadmin -sk:minioadmin -s:http://192.168.0.4:9090 -b:testbucket -f:true -sv:2.0 -t:d:\Test`

### acl命令

#### acl get 命令

> 获取bucket的权限信息

- `-b|--bucket` 使用的bucket名
- `-k|--key` s3文件的key

> 例如: `s3-cli acl  -b:testbucket -k:123456.txt`

### list_bucket命令

- 列出当前ak,sk下全部的bucket: `s3-cli list_bucket`

### list 命令

> 列出指定数量的对象信息

- `-b|--bucket` 使用的bucket名
- `-m|--max` 列出的bucket数量(最大的数量为1000)
- `-p|--prefix` 前缀
- `-d|--delimiter` 分隔符

> 例如: `s3-cli list -b:testbucket -m:10 -prefix:1003/200 -d:/`

### upload 命令

> 上传文件(超过5MB的文件自动分片)

- `-b|--bucket` 使用的bucket名
- `-p|--path` 上传的文件的路径(可以指定多个上传路径 `-p:1.txt -p:2.txt ...`)
- `-a|--autodel` 上传后是否自动删除(默认为true)
- `-d|--dir` 指定目录上传 (`-p` 与 `-d` 参数二选一)

> 例如: `s3-cli upload -b:testbucket -p:1.txt -p:2.txt -a:true` 或 `s3-cli upload -b:testbucket -a:true -d:D:\Test1`

### upload_default 命令

> 上传指定大小的文件(自动生成文件数据上传,超过5MB的文件自动分片)

- `-b|--bucket` 使用的bucket名
- `-a|--autodel` 上传后是否自动删除(默认为true)
- `-s|--size` 文件的大小

> 例如: `s3-cli upload_default -b:testbucket -a:true -s:4096`

### download 命令

> 下载指定的文件到临时目录

- `-b|--bucket` 使用的bucket名
- `-k|--key` 下载文件的key,可以指定多个

> 例如: `s3-cli download -b:testbucket -k:1000/1.txt -k:1000/2.txt`

### del 命令

> 删除指定的文件

- `-b|--bucket` 使用的bucket名
- `-k|--key` 删除文件的key,可以指定多个

> 例如: `s3-cli del -b:testbucket -k:1000/1.txt -k:1000/2.txt`

### copy 命令

> 拷贝文件

- `-sb|--sourcebucket` 源bucket名
- `-db|--destbucket` 目标bucket名
- `-sk|--sourcekey` 源文件的key
- `-dk|--destkey` 目标文件的key

> 例如: `s3-cli copy -sb:testbucket -db:testbucket2 -sk:1000/1.txt -dk:1000/2.txt`

### gen 命令

> 生成文件的临时访问地址

- `-b|--bucket` 使用的bucket名
- `-k|--key` 文件的key
- `-t|--expires` 文件有效期的秒数(多少秒后过期)

> 例如: `s3-cli gen -b:testbucket -k:1000/1.txt -t:600`

### speed 命令

> 测速命令,测试单线程下上传,下载的速度

- `-b|--bucket` 使用的bucket名
- `-s|--size` 生成测试文件大小
- `-c|--count` 测试文件数量
- `-a|--autodel` 是否自动删除测试数据(默认为true)

> 例如: `s3-cli speed -b:testbucket -s:10240 -c:100 -a:true`
