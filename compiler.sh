#!/bin/bash

# User's CDPATH can interfere with cd in this script
unset CDPATH
# Get the true name of this script
script="`test -L "$0" && readlink -n "$0" || echo "$0"`"
dir="$PWD"
cd "`dirname "$script"`"

if [ \( $# -gt 1 \) -o \( "$1" = "-h" \) -o \( "$1" = "--help" \) ]
then
	echo "Usage:"
	echo "    `basename "$0"`              Compiles all .jack files in the current"
	echo "                                 working directory."
	echo "    `basename "$0"` DIRECTORY    Compiles all .jack files in DIRECTORY."
	echo "    `basename "$0"` FILE.jack    Compiles FILE.jack to FILE.vm."
else
	if [ $# -eq 0 ]
	then
		# Use current directory as arg1
		arg1="$dir"
	else
		# Convert arg1 to an absolute path
		if [ `echo "$1" | sed -e "s/\(.\).*/\1/"` = / ]
		then
			arg1="$1"
		else
			arg1="$dir/$1"
		fi
	fi
    for file in $(ls $arg1/*.jack) ; do 
        echo "Compiling $(basename $file)"
        python compiler.py $file > ${file/jack/vm} || exit $?
        python compiler.py $file --xml > ${file}.xml || exit $?
    done
	argbase=$(basename $arg1)
	echo "Assembling ${argbase}"
	python translator.py $arg1/*.vm > $arg1/$argbase.asm || exit $?
	python translator.py $arg1/*.vm --xml > $arg1/$argbase.vm.xml || exit $?
	python assembler.py $arg1/$argbase.asm > $arg1/$argbase.hack || exit $?
fi
