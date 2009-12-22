Overview
========

Pithy is a static web resource manager for ASP.NET and ASP.NET MVC web applications. It
utilizes a configuration API to store stylesheet, javascript, and static file resources
by tag names. Depending on how Pithy is configuring, requesting a tag name may combine
and compress multiple resources into a single file.

Pithy utilizes the YUI JavaScript and Stylesheet compression engine.

Pithy also includes a plugin architecture which will let you process Stylesheets
and JavaScript files before they are combined and compressed. Included with Pithy
are a few useful plugins; one which will compile dotLess-style Stylesheets, and one
which can integrate with a simple cache-busting engine.


Change History
==============

2009.12.22.1000:
    First release of the code base and compiled binaries.
