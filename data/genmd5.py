#!/usr/bin/python

'''
/*============================================+
|		((__))								  |
|		 (00)								  |
|  -nn--(o__o)--nn------Code By Rockie X.Lee  |
+============================================*/
'''

import os
import sys
import hashlib


workdir = "."
output = "md5.txt"
md5file = None

def iscore(fname):
	#if fname.find("bonus") >= 0:
		return "1"
	#else:
	#	return ""

def md5sum(fname):
	return hashlib.md5(open(fname, 'rb').read()).hexdigest()

def collectmd5(fname, lines):
	if os.path.isdir(fname):
		files = os.listdir(fname)
		for f in files:
			if f[0] != '.':
				collectmd5(fname + "/" + f, lines)
	elif os.path.isfile(fname) and os.path.splitext(fname)[1][1:] != "meta" and os.path.splitext(fname)[1][1:] != "manifest":
		newFileName = fname
		if fname.find(workdir) == 0:
			newFileName = fname[len(workdir)+1:]
		lines.append("%s,%32s,%s,%d\n" % (newFileName, md5sum(fname), iscore(fname), os.path.getsize(fname)))

if __name__ == '__main__':
	workdir = "data"
	md5file = open(output, "w+")
	lines = []
	collectmd5(workdir, lines)
	with open(output, 'w') as outputf:
		outputf.writelines(lines)
	manifest = "manifest.txt"	
	version = 1
	md5md5 = md5sum(output)
	if os.path.isfile(manifest) :
		mf = open(manifest, "r")
		ver,md5 = mf.readline().split(",")
		if md5 != md5md5:
			version = int(ver) + 1
		else:
			version = int(ver)
		mf.close()
	with open(manifest, "w") as omf:
		omf.write("%d,%s" % (version, md5md5))

