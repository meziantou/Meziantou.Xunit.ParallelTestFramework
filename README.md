By default, all test cases in a test class share the same collection instance which ensures they run synchronously.
This package automatically attribute a unique collection to all test cases, so they can run in parallel.

The code is greatly inspired by @tmort93: https://github.com/xunit/xunit/issues/1986#issuecomment-831322722