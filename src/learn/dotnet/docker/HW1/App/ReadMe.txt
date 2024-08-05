dotnet new console -o App -n DotNet.Docker

REM test by "dotnet run -- 5"

docker build -t counter-image -f Dockerfile .

docker images

docker create --name core-counter counter-image
docker ps -a

docker start core-counter
docker ps
docker attach --sig-proxy=false core-counter

docker stop core-counter

docker rm core-counter