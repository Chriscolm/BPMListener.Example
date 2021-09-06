import os;
import sys;

def do_short_process(args):    
    if(args is not None and len(args)) > 1:
       print("args list:")
       for arg in args:
           print(arg)
           if(arg == "666"):
               raise ValueError("value out of range")



if __name__ == "__main__":
    args = sys.argv;  
    try:
        print("short process called")
        do_short_process(args)
        print("short process finished")
        sys.exit(0);
    except ValueError:
        print("short process ended with value error")
        sys.exit(-1)
