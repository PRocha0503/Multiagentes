from mesa import Agent

class TrafficLightAgent(Agent):
    """
    Agent class for traffic
    input: unique id, model
    output: traffic agent
    """
    def __init__(self,id,model,onId,type) -> None:
        super().__init__(id,model)
        self.status = False if onId == 1 else True
        self.type = type
        self.counter = 0
        self.dir = ""
        
    def calculateCounter(self):
        """
        Calculate the counter
        """
        sign = 1 if self.dir[0] == "-" else -1
        dirXorY = self.dir[1:]
        if dirXorY == "x":
            for i in range(1,5):
                r = self.pos[0]+(i*sign)
                if r < self.model.grid.width and r >= 0:
                    contents = self.model.grid.get_cell_list_contents([(r,self.pos[1])])
                    if len(contents) > 1:
                        self.counter += 1
        else:
            for i in range(1,5):
                r = self.pos[1]+(i*sign)
                if r < self.model.grid.height and r >= 0:
                    contents = self.model.grid.get_cell_list_contents([(self.pos[0],r)])
                    if len(contents) > 1:
                        self.counter += 1


    def step(self):
        """
        Step function
        """
        self.calculateCounter()
        

    def getStatusColor(self):
        """
        Get the status of the traffic light
        input: none
        output: boolean
        """
        if self.status:
            return "green"
        else:
            return "red"
