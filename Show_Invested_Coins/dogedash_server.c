#include <sys/socket.h>
#include <netinet/in.h>
#include <netinet/tcp.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <errno.h>
#include <string.h>
#include <strings.h>
#include <signal.h>
#include <sys/types.h>
#include <time.h> 
#include <fcntl.h>
#include <syslog.h>
#include <sys/ipc.h> 
#include <stdbool.h>
#include <inttypes.h>

int logfile(char* message) {
	char buffer[4096];
	
	setlogmask(LOG_UPTO (LOG_NOTICE));
	openlog("dogedash_server", LOG_PID | LOG_NDELAY, LOG_LOCAL1);
	//syslog (LOG_NOTICE, "Program started by User %d", getuid ());
	snprintf((char *)buffer, sizeof(buffer), "[%s]", message);
	syslog(LOG_NOTICE, buffer);
	closelog();
}

void ctrlc_handler(int sig)
{
	// printf("CTRL-C pressed!\n");
	// TODO: Socket schlieﬂen etc....
	exit(0);
}

void sigpipe_handler(int unused)
{
	logfile("Broken pipe: Client disconnected!");
	logfile("Terminating child thread now...");
	_exit(1);
}

void write_to_dd_file(char* filename) {
	FILE *pFile;
	time_t timer;
	char buffer[50];
	char tmp[60];
	struct tm* tm_info;
	timer = time(NULL);
	tm_info = localtime(&timer);
	
	snprintf(tmp, sizeof(tmp), "%s.txt", filename);
	pFile=fopen("%s", "a");

	if(pFile==NULL) {
		printf("Error opening file.");
	}
	else {
			strftime(buffer, sizeof(buffer), "%d.%m.%Y um %H:%M:%S Uhr", tm_info);
			snprintf(tmp, sizeof(tmp), "Klingel gedr\201ckt am %s\r\n", buffer);
			fprintf(pFile, "%s", tmp);
		}
	fclose(pFile);
}

int main(int argc, char *argv[])
{
    int listenfd = 0, newsockfd = 0;
    struct sockaddr_in serv_addr;
    char sendBuff[1025];
    time_t ticks; 
	int read_size;
	int fd, bytes;
    unsigned char data[3];
	char client_message[2000];
	int counter;
	int pid; // Handle multiple clients
	int addrlen = sizeof(serv_addr);
	int opt = 1;
	
	logfile("Started");
	setvbuf(stdout, NULL, _IONBF, 0);
    printf("DogeDash-Server v1.0\n");
	
	sigaction(SIGPIPE, &(struct sigaction){sigpipe_handler}, NULL);

	// CTRL + C: end mouse_shared program
	signal(SIGINT, ctrlc_handler);
	
    listenfd = socket(AF_INET, SOCK_STREAM, 0);
    memset(&serv_addr, '0', sizeof(serv_addr));
	memset(sendBuff, '0', sizeof(sendBuff)); 
    serv_addr.sin_family = AF_INET;
    serv_addr.sin_addr.s_addr = INADDR_ANY;
    serv_addr.sin_port = htons(7070); 

	if (setsockopt(listenfd, SOL_SOCKET,
				SO_REUSEADDR | SO_REUSEPORT, &opt,
				sizeof(opt))) {
		perror("setsockopt");
		exit(EXIT_FAILURE);
	}
	
	if (bind(listenfd, (struct sockaddr*)&serv_addr,
			sizeof(serv_addr))
		< 0) {
		perror("bind failed");
		exit(EXIT_FAILURE);
	}
	
	if (listen(listenfd, 3) < 0) {
		perror("listen");
		exit(EXIT_FAILURE);
	}
	
    while(1)
    {
		newsockfd = accept(listenfd, (struct sockaddr*)&serv_addr, (socklen_t*)&addrlen);	
		if (newsockfd < 0) {
			logfile("ERROR on accept!");
			return -1;
		}
		//fork new process
        pid = fork();
        if (pid < 0) {
			logfile("ERROR in new process creation");
        }
        if (pid == 0) {
			//child process
			close(listenfd);
			//do whatever you want
			////////////////////

			int n = 0;
			char buffer[256];
			bzero(buffer,256);
			int client_bytes = 0;
			char tmp[50];
			
			if((client_bytes = read(newsockfd,buffer,256)) > 0) {
				// Before sending, test if client request was "want_bell"
				/*if(strcmp(buffer, "want_bell") == 0) {  // Want to have bell-signals
					bzero(buffer,256);
					// reset, because memory could have the last door-pressed bell
					write_to_memory("CLEARED");
				}
				else {
					logfile("want_bell_NOT_there");
				}*/
				snprintf(tmp, sizeof(tmp), "Returned from client: [%s]\r\n", buffer);
				logfile(tmp);
			}
			else if(client_bytes == 0) {
				logfile("Client disconnected");
				break;
			}
			else {
				logfile("Client read failed");
			}
			////////////////////
            close(newsockfd);
        } else {
			//parent process
			close(newsockfd);
        }
    }
}
