cmake_minimum_required(VERSION 3.12)
project(test C)

set(CMAKE_C_STANDARD 11)

add_executable(test
        lisk32.c
        lisk32.h
        test.c)
