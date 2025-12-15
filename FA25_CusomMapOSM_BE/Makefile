start:
	dotnet watch --launch-profile dev

build:
	dotnet build

test:
	dotnet test

redislocal:
	docker-compose up -d

docker:
	docker build -t imos_server -f ./CusomMapOSM_API/Dockerfile .

release:
	docker build --build-arg APP_VERSION=$(version) -t imos_server -f ./CusomMapOSM_API/Dockerfile .
	docker tag imos_server:latest imosregistry.azurecr.io/imos_server:$(version)
	docker push imosregistry.azurecr.io/imos_server:$(version)
deploy:
	@powershell -ExecutionPolicy Bypass -File scripts/deploy.ps1
new-migration:
	dotnet ef migrations add $(name) --project CusomMapOSM_Infrastructure --startup-project CusomMapOSM_API --verbose

migration:
	dotnet ef database update --project CusomMapOSM_Infrastructure --startup-project CusomMapOSM_API

push-registries:
	docker tag imosregistry.azurecr.io/imos_server:$(version)
	docker push imosregistry.azurecr.io/imos_server:$(version)

.PHONY: start build docker new-migration migration test release