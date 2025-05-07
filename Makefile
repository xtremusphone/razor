
export AWS_ACCESS_KEY_ID ?= test
export AWS_SECRET_ACCESS_KEY ?= test
export ACCOUNT_ID = 000000000000
export AWS_DEFAULT_REGION = ap-southeast-4
DOCKER_CONTAINER = localstack_nginx_1_0

usage:       ## Show this help
	@fgrep -h "##" $(MAKEFILE_LIST) | fgrep -v fgrep | sed -e 's/\\$$//' | sed -e 's/##//'

install:     ## Install dependencies
	@which localstack || pip install localstack
	@which awslocal || pip install awscli-local

run:      ## Build and deploy the app locally
		awslocal ecr delete-repository --repository-name razor --force; \
		echo "Creating a new ECR repository locally"; \
		repoUri=$$(awslocal ecr create-repository --repository-name razor | jq -r '.repository.repositoryUri'); \
		echo "Building the Docker image, pushing it to local ECR URL: $$repoUri"; \
		sleep 3; \
		docker build -t "$$repoUri" .; \
		docker push "$$repoUri"; \
		docker rmi "$$repoUri"; \
		echo "Creating ECS infrastructure locally"; \
		awslocal cloudformation delete-stack --stack-name razor-infra; \
		awslocal cloudformation create-stack --stack-name razor-infra --template-body file://AWS/ecs.infra.yml && \
		awslocal cloudformation wait stack-create-complete --stack-name razor-infra && \
		echo "Deploying ECS app to local environment"; \
		awslocal cloudformation delete-stack --stack-name razor; \
		awslocal cloudformation create-stack --stack-name razor \
			--template-body file://AWS/ecs.sample.yml \
			--parameters ParameterKey=ImageUrl,ParameterValue=$$repoUri && \
		awslocal cloudformation wait stack-create-complete --stack-name razor && \
		echo "ECS app successfully deployed. Trying to access app endpoint." && \
		cluster_arn=$$(awslocal ecs list-clusters | jq -r '.clusterArns[0]') && \
		for i in {1..5}; do task_arn=$$(awslocal ecs list-tasks --cluster $$cluster_arn | jq -r '.taskArns[0]'); if [ "$$task_arn" != "null" ]; then break; fi; sleep 2; done && \
		for i in {1..5}; do app_port=$$(awslocal ecs describe-tasks --cluster $$cluster_arn --tasks $$task_arn | jq -r '.tasks[0].containers[0].networkBindings[0].hostPort'); if [ "$$app_port" != "null" ]; then break; fi; sleep 2; done && \
		lb_public_url=$$(awslocal cloudformation describe-stacks --stack-name razor-infra | jq -r '.Stacks[0].Outputs | map(select(.OutputKey =="ExternalUrl"))[0].OutputValue') && \
		echo "curling $$lb_public_url:$$app_port" && \
		curl $$lb_public_url:$$app_port && \
		echo "App successfully deployed. You can access it under http:://$$lb_public_url:$$app_port"

clean:        ## stop and remove created containers
	@((docker stop $(DOCKER_CONTAINER) && docker rm $(DOCKER_CONTAINER))>/dev/null 2>/dev/null) ||:

start:
	localstack start -d

stop:
	@echo
	localstack stop
ready:
	@echo Waiting on the LocalStack container...
	@localstack wait -t 30 && echo Localstack is ready to use! || (echo Gave up waiting on LocalStack, exiting. && exit 1)

logs:
	@localstack logs > logs.txt

test-ci:
	make start install ready run; return_code=`echo $$?`;\
	make logs; make stop; exit $$return_code;
	
.PHONY: usage install start run stop clean ready logs test-ci