#!/bin/bash

# Update target framework to .NET 8.0 for .NET 7.0 projects
find . -name "*.csproj" -type f -exec sed -i '' 's/net7.0/net8.0/g' {} \;

# Update LangVersion from 11 to 12
find . -name "*.csproj" -type f -exec sed -i '' 's/<LangVersion>11<\/LangVersion>/<LangVersion>12<\/LangVersion>/g' {} \;

echo "Target frameworks and LangVersions updated successfully!"
