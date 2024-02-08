#!/bin/bash

CURRENT_VERS=$(sed '5!d' CHANGELOG.md | cut -d " " -f 2)
echo "The Current version is $CURRENT_VERS, enter new version."
read -p "Version: " -i $CURRENT_VERS -e CHOSEN_VERS
CHOSEN_VERS=${CHOSEN_VERS:-$CURRENT_VERS}
echo "Will publish as version: $CHOSEN_VERS"

echo "Please enter a changelog and commit message. An empty message will only change version number and not add a changelog entry."
read -p "Message: " MESSAGE

if [[ -z "$MESSAGE" ]]; then
   echo "Empty message, will update in place."
   sed -i 's|## '$CURRENT_VERS'|## '$CHOSEN_VERS'|' CHANGELOG.md
   sed -i 's|### '$CURRENT_VERS'|### '$CHOSEN_VERS'|' README.md
   COMMIT_MESSAGE="Bumped to version ${CHOSEN_VERS}"
else
   echo "Message, will add changelog entry."
   set -o noglob
   set +o histexpand
   sed -i ''$(grep -Fn '## '$CURRENT_VERS'' CHANGELOG.md | cut -d ":" -f 1)'i## '$CHOSEN_VERS'\n\n- '"$MESSAGE"'\n' CHANGELOG.md
   replace '(## Release Notes(?:(?:.|\n)*?))(###(?:.|\n)*)' '$1' README.md
   echo -e "### $CHOSEN_VERS\n\n- $MESSAGE" >> README.md
   set +o noglob
   set -o histexpand
   COMMIT_MESSAGE="$MESSAGE; bumped to version ${CHOSEN_VERS}"
fi

git add .
git commit -m "$COMMIT_MESSAGE"
git push
git tag v$CHOSEN_VERS HEAD
git push --tags

set -o noglob
set +o histexpand
gh release create -t "Release v$CHOSEN_VERS" -n "$MESSAGE" v$CHOSEN_VERS
set +o noglob
set -o histexpand

docker buildx build --build-arg CONFIG="Release" --platform=linux/amd64,linux/arm64 --push -t "mattmckenzy/toothpick:latest" -t "mattmckenzy/toothpick:$CHOSEN_VERS" -f "ToothPick/Dockerfile" .
docker run -v .:/workspace \
  -e DOCKERHUB_USERNAME='mattmckenzy' \
  -e DOCKERHUB_PASSWORD=$(cat ~/.tokens/docker-hub) \
  -e DOCKERHUB_REPOSITORY='mattmckenzy/toothpick' \
  -e README_FILEPATH='/workspace/README.md' \
  peterevans/dockerhub-description:latest