// stdafx.h : Standard system include file

#pragma once


#define WIN32_LEAN_AND_MEAN	
#include <stdio.h>

#ifdef __APPLE__
#include <string.h>
#include <unistd.h>
#endif

#ifdef __APPLE__
#define SLEEP(_x_) (usleep(_x_ * 1000))
#else
#define SLEEP(_x_) (Sleep(_x_))
#endif
