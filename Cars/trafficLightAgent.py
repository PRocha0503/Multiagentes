from mesa import Agent

class TrafficLightAgent(Agent):
    """
    Agent class for traffic
    input: unique id, model
    output: traffic agent
    """
    def __init__(self,id,model,onId) -> None:
        super().__init__(id,model)
        self.status = False if onId == 1 else True
    
    def step(self):
        """
        Step function
        """
        if self.model.schedule.time % 10 == 0:
            self.status = not self.status

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
