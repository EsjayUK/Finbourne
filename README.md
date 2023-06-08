# Finbourne
This code contains the component, unit tests and integration tests to prove the viability of the component

Due to time restrictions, the tests when run together will have issues because of the singleton component.  I have not had the time to work out and code a work around but I do have an idea.  

This is based on having a method to reinitialise the cache that can be called at the beginning of from every test but this would also expose the method to users of the component that I wasn't happy about.

So for the time being, the tests, unit or integration, can be run individually to check the success of it.

Thanks
Saj
