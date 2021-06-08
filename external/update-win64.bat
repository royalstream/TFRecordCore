@echo off
curl https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/example/feature.proto --output tensorflow\core\example\feature.proto
curl https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/example/example.proto --output tensorflow\core\example\example.proto
protoc-3.17.1-win64\protoc -I=. --csharp_out=. tensorflow\core\example\feature.proto
protoc-3.17.1-win64\protoc -I=. --csharp_out=. tensorflow\core\example\example.proto
