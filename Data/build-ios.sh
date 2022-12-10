#!/usr/bin/env sh

while getopts s: OPT
do
    case ${OPT} in
        s)
            PROJECT_DIR=${OPTARG};;
    esac
done

if [ -z "${PROJECT_DIR}" ]; then
    PROJECT_DIR=$(pwd)
fi

BASE_PATH="${PROJECT_DIR}/Builds/iOS"
ARCHIVE_PATH="${BASE_PATH}/archive.xcarchive"

# アーカイブの作成
xcodebuild \
    archive \
    -scheme "Unity-iPhone" \
    -workspace "${BASE_PATH}/Unity-iPhone.xcworkspace" \
    -archivePath "${ARCHIVE_PATH}" \
    -allowProvisioningUpdates \
    -destination 'generic/platform=iOS' \
    CODE_SIGN_STYLE="Auto" \
    -quiet

mkdir -p "./Build"

# アーカイブからipaへ書き出し
xcodebuild \
    -exportArchive \
    -archivePath "${ARCHIVE_PATH}" \
    -exportPath "${PROJECT_DIR}/Build/" \
    -exportOptionsPlist "${PROJECT_DIR}/Data/ExportOptions/ExportOptions.plist" \
    -quiet 
