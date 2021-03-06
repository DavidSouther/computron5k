/********************************************************************************
 * The contents of this file are subject to the GNU General Public License      *
 * (GPL) Version 2 or later (the "License"); you may not use this file except   *
 * in compliance with the License. You may obtain a copy of the License at      *
 * http://www.gnu.org/copyleft/gpl.html                                         *
 *                                                                              *
 * Software distributed under the License is distributed on an "AS IS" basis,   *
 * without warranty of any kind, either expressed or implied. See the License   *
 * for the specific language governing rights and limitations under the         *
 * License.                                                                     *
 *                                                                              *
 * This file was originally developed as part of the software suite that        *
 * supports the book "The Elements of Computing Systems" by Nisan and Schocken, *
 * MIT Press 2005. If you modify the contents of this file, please document and *
 * mark your changes clearly, for the benefit of others.                        *
 ********************************************************************************/

package org.nand2tetris.hack.core.parts;

import java.util.Vector;
import org.nand2tetris.hack.core.events.ErrorEvent;
import org.nand2tetris.hack.core.events.ErrorEventListener;

/**
 * An interactive computer part - a computer part that enables input to its GUI.
 * This is the abstract base class for all interactive computer parts.
 * This computer part notifies its listeners on errors using the ComputerPartErrorEvent.
 * It also listens to ComputerPartGUIErrorEvents from the GUI (and therefore should register
 * as a ComputerPartGUIErrorEventListener to it). When such an event occurs,
 * the error is sent to the error listeners of the computer part itself.
 */
public abstract class InteractiveComputerPart extends ComputerPart
 implements ErrorEventListener {

    private Vector<ComputerPartErrorEventListener> errorListeners;

    /**
     * Constructs a new interactive computer part.
     * If hasGUI is true, the ComputerPart will display its contents.
     */
    public InteractiveComputerPart(boolean hasGUI) {
        super(hasGUI);
        errorListeners = new Vector<>();
    }

    /**
     * Registers the given ComputerPartErrorEventListener as a listener to this ComputerPart.
     */
    public void addErrorListener(ComputerPartErrorEventListener listener) {
        errorListeners.addElement(listener);
    }

    /**
     * Un-registers the given ComputerPartErrorEventListener from being a listener
     * to this ComputerPart.
     */
    public void removeErrorListener(ComputerPartErrorEventListener listener) {
        errorListeners.removeElement(listener);
    }

    /**
     * Notifies all the ComputerPartErrorEventListeners on an error that occurred in the
     * computer part by creating a ComputerPartErrorEvent (with the error message)
     * and sending it using the computerPartErrorOccurred method to all the listeners.
     */
    public void notifyErrorListeners(String errorMessage) {
        ComputerPartErrorEvent event = new ComputerPartErrorEvent(this, errorMessage);

        for (ComputerPartErrorEventListener listener: errorListeners)
            listener.computerPartErrorOccurred(event);
    }

    /**
     * Clears all the ComputerPartErrorEventListeners from errors.
     */
    public void clearErrorListeners() {
        ComputerPartErrorEvent event = new ComputerPartErrorEvent(this, null);

        for (ComputerPartErrorEventListener listener: errorListeners)
            listener.computerPartErrorOccurred(event);
    }

    /**
     * Called when an error occurred in the GUI.
     * The event contains the source object and the error message.
     */
    public void errorOccurred(ErrorEvent event) {
        notifyErrorListeners(event.getErrorMessage());
    }
}
