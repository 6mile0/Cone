from fastapi import FastAPI, UploadFile, File
from fastapi.responses import JSONResponse
import subprocess
import os
import shutil

app = FastAPI()

@app.get("/")
async def root():
    return {"message": "Hello World"}

@app.post("/deploy")
async def deploy(
    tar_file: UploadFile = File(...),
    compose_file: UploadFile = File(...)
):
    tar_path = "/tmp/app.tar"
    compose_path = "/tmp/docker-compose.yml"

    try:
        # tarファイルを保存
        with open(tar_path, "wb") as f:
            f.write(await tar_file.read())

        # docker-compose.ymlを保存
        with open(compose_path, "wb") as f:
            f.write(await compose_file.read())

        # Dockerイメージロード
        subprocess.run(
            ["docker", "load", "-i", tar_path],
            check=True,
            capture_output=True,
            text=True
        )

        # Docker Composeでデプロイ
        subprocess.run(
            ["docker-compose", "-f", compose_path, "up", "-d"],
            check=True,
            capture_output=True,
            text=True
        )

    except subprocess.CalledProcessError as e:
        error_msg = {
            "status": "error",
            "command": " ".join(e.cmd) if isinstance(e.cmd, list) else e.cmd,
            "returncode": e.returncode,
            "stdout": e.stdout,
            "stderr": e.stderr
        }
        return JSONResponse(content=error_msg, status_code=500)
    except Exception as e:
        error_msg = {
            "status": "error",
            "message": str(e)
        }
        return JSONResponse(content=error_msg, status_code=500)
    finally:
        # 一時ファイルを削除
        for path in [tar_path, compose_path]:
            if os.path.exists(path):
                os.remove(path)

    return JSONResponse(content={"status": "ok"}, status_code=200)

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=9000)
