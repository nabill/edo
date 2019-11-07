pipeline {
    agent { label 'master' }

    environment {
        URL_REGISTRY = 'registry.dev.happytravel.com'
        APP_NAME = 'edo-api'
        IMAGE_NAME = 'edo-api:dev'
        NAMESPACE = 'dev'
    }

    stages {
        stage('Checkout happytravel repository') {
            steps {
                dir('docker/edo-api') {
                    git branch: 'master', credentialsId: 'bitbucket', url: 'git@bitbucket.org:happytravel/edo-api.git'
                }
            }
        }
        stage('Force login at docker registry') {
            steps {
                sh 'docker login https://$URL_REGISTRY -u username -p password'
            }
        }
        stage('Build docker image') {
            steps {
                dir('docker/edo-api') {
                    withCredentials([string(credentialsId: 'VAULT_TOKEN', variable: 'VAULT_TOKEN')]) {
                        sh 'docker build -t $URL_REGISTRY/$IMAGE_NAME-$BUILD_NUMBER --build-arg "VAULT_TOKEN=$VAULT_TOKEN" . --no-cache'
                    }
                }
            }
        }
        stage('Push docker image to repository') {
            steps {
                sh 'docker push $URL_REGISTRY/$IMAGE_NAME-$BUILD_NUMBER'
                sh 'docker tag $URL_REGISTRY/$IMAGE_NAME-$BUILD_NUMBER $URL_REGISTRY/$IMAGE_NAME-latest'
                sh 'docker push $URL_REGISTRY/$IMAGE_NAME-latest'                
            }
        }
        stage('Deploy to k8s') {
            steps {
                dir('docker/edo-api/Helm') {
                    withCredentials([file(credentialsId: 'k8s', variable: 'k8s_cred')]) {
                        sh './setRevision.sh $BUILD_NUMBER'
                        sh 'helm --kubeconfig /$k8s_cred upgrade --install $NAMESPACE-$APP_NAME -f values_dev.yaml --wait --namespace $NAMESPACE ./'
                    }
                }
            }
        }        
    }
    post {
        always {
            echo 'One way or another, I have finished'
            deleteDir() /* clean up our workspace */
        }
        success {
            echo 'I succeeeded!'
            discordSend description: 'Job:'+' '+env.JOB_NAME+', build number: '+env.BUILD_NUMBER, footer: 'SUCCESSFUL', link: env.BUILD_URL, result: currentBuild.currentResult, title: JOB_NAME+' env: '+env.NAMESPACE, webhookURL: 'https://discordapp.com/api/webhooks/585188681892233239/eFnBXVIb-03zxCqOncAkCXvbnke02dWsDx2acpFDp1Lhe7JUyW5jGahAIH2VaiqzAbUQ'
        }
        unstable {
            echo 'I am unstable :/'
        }
        failure {
            echo 'I failed :('
            discordSend description: 'Job:'+' '+env.JOB_NAME+', build number: '+env.BUILD_NUMBER, footer: 'FAILED', link: env.BUILD_URL, result: currentBuild.currentResult, title: JOB_NAME+' env: '+env.NAMESPACE, webhookURL: 'https://discordapp.com/api/webhooks/585188681892233239/eFnBXVIb-03zxCqOncAkCXvbnke02dWsDx2acpFDp1Lhe7JUyW5jGahAIH2VaiqzAbUQ'
        }
        changed {
            echo 'Things were different before...'
        }
    }
}