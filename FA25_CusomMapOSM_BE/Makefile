start:
	dotnet watch --launch-profile dev

build:
	dotnet build

test:
	dotnet test

redislocal:
	docker-compose up -d

docker:
	docker build -t lms_server -f ./CusomMapOSM_API/Dockerfile .

release:
	docker build --build-arg APP_VERSION=$(version) -t lms_server -f ./CustomMapOSM_API/Dockerfile .
	docker tag /lms_server:$(version)
	docker push /lms_server:$(version)

new-migration:
	dotnet ef migrations add $(name) --verbose 

migration:
	dotnet ef database update

push-registries:
	docker tag /lms_server:$(version)
	docker push /lms_server:$(version)

.PHONY: start build docker new-migration migration test release