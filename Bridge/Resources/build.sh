#! /bin/bash

# TODO: Stop using JSBuild on windows and sed on linux and use internal
#       Microsoft.Ajax.Utilities.Minifier to minify JavaScript code.
#       This is already done during translation and left just for transition.

jsbfile="Bridge.jsb"
jsfile="bridge.js"
jsminf="bridge.min.js"
ts="$(date +%Y%m%d%H%M%S)"
tmpbrjs="bridge-${ts}.js"

echo "$(date) - Building ${jsfile} and ${jsminf}"
if [ -d ../../Resources ]; then
 cd ../../Resources
else
 if [ ! -e "${jsbfile}" ]; then
  echo "Unable to move to ../../Resources. Current WD: '$(pwd)'."
  exit 1
 fi
 # else, we're probably running build.sh from the directory itself
fi

if [ ! -e "${jsbfile}" ]; then
 echo "Unable to locate '${jsbfile}'."
 exit 1
fi

# In order to try to preserve a single sed command, will use extended regexp
# which is triggered with '-r' on GNU sed and '-E' on OS X.
# Extended regexp is more similar between the two sed versions.
if [ "$(uname -s)" == "Darwin" ]; then
 osx=true
 sedopts="-E" # sed's option to extended regexp

 # sed from OS X needs escape chars specified this way in order to work.
 sednl="\\
" # 'echo -ne' trick does not work for newline
 sedBOM="$(echo -ne "\xef\xbb\xbf")"
else
 osx=false
 sedopts="-r" # sed's option to extended regexp

 sednl="\n"
 sedBOM="\xef\xbb\xbf"
fi

sedcmd="sed ${sedopts}"

fileheader="/*
 * $(grep 'copyright *=' "${jsbfile}" | head -n1 | ${sedcmd} "s/.*copyright *= *\"([^\"]+)\".*/\1/g;s/&#xD;&#xA;/${sednl} * /g")
 */"

filelist="$(cat "${jsbfile}" | grep "<include name=" | ${sedcmd} "s/^.*<include name=\"(.*)\" *\/>.*\$/\1/")"

echo "Merging $(echo "${filelist}" | wc -l) files."
if [ -e "${tmpbrjs}" ]; then
 rm "${tmpbrjs}" || {
  echo "Unable to remove '${tmpbrjs}'."
  exit 1
 }
fi

# Add BOM to the beginning of the file
echo -ne "\xef\xbb\xbf" >> "${tmpbrjs}"

# Insert header
echo "${fileheader}" >> "${tmpbrjs}"

for file in ${filelist}; do
 if [ -e "${file}" ]; then
#  echo "
#// @source ${file}
#" >> "${tmpbrjs}"
  ${sedcmd} "1s/^${sedBOM}//" "${file}" >> "${tmpbrjs}"
  echo "" >> "${tmpbrjs}" # add EOL at EOF. :)
 else
  echo "File '${file}' not found."
  exit 1
 fi
done

cp "${tmpbrjs}" "${jsfile}"
rm "${tmpbrjs}"

echo "$(date) - Done building ${jsfile}."
