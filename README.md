# boolean-retrieval

A simple boolean retrieval search engine that searches the dataset for a given query and returns the relevant documents names.

Boolean operations AND, OR and NOT are also implemented. 
For e.g. you can specify queries following the format given below:
Laptops  
Laptops AND Dell
Laptops OR Mobiles
NOT Laptops

The dataset I am using is following
http://www.cs.cmu.edu/afs/cs/project/theo-20/www/data/news20.tar.gz

Format to run the program

open cmd

enter
ConsoleApplication2 "query" "fullpath to the folder"

for e.g.

ConsoleApplication2 "England AND Canada" "C:\20_newsgroups"
