MaerskApi Poul Eker 04.05.2022.

The solution consists of two projects, the API (MaerskApi) and the test (TestMaerskApi)

On startup, the Api opens on a swagger page, from where it also can be tested, with the specified parameters.

Currencies allowed:

        GBP,
        USD,
        EUR

Exhangerates for each of these are loaded manualy and are readonly.

There are no other data preloaded.




3. party Packages installed:


Swashbuckle.AspNet.Core 6.3.1	https://github.com/domaindrivendev/Swashbuckle.AspNet.Core

Moq 4.17.2
		https://github.com/moq/moq4

NUnit 3.13.3
		https://nunit.org

NUnit3TestAdapter 4.2.1
	https://docs.nunit.org/articles/vs-test-adapter/index.html

