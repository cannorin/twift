﻿namespace Twift
open System

module Settings =
  begin
    let isVerbose = ref true
    let isDm = ref false
    let dmTarget = ref ""
    let isReceive = ref false
    let receiveIds = ref (0L, 0L)
    let doesShare = ref true
  end