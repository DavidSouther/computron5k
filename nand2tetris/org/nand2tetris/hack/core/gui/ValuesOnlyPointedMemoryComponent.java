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

package org.nand2tetris.hack.core.gui;

import javax.swing.table.DefaultTableCellRenderer;
import javax.swing.table.TableModel;

/**
 * A Memory component that has an address pointer and displays only its values
 * (no addresses).
 */
public class ValuesOnlyPointedMemoryComponent extends PointedMemoryComponent {

    private static final long serialVersionUID = 2993533211412414297L;

    /**
     * Returns the table model of this component.
     */
    protected TableModel getTableModel() {
        return new ValuesOnlyPointedMemoryTableModel();
    }

    protected DefaultTableCellRenderer getCellRenderer() {
        return new ValuesOnlyPointedTableCellRenderer();
    }

    /**
     * Returns the index of the values column.
     */
    protected int getValueColumnIndex() {
        return 0;
    }

    // Determines the width of each column in the table.
    protected void determineColumnWidth() {}

    // An inner class representing the model of the table.
    public class ValuesOnlyPointedMemoryTableModel extends MemoryTableModel {
        private static final long serialVersionUID = -2343948261733719872L;

        /**
         * Returns the number of columns.
         */
        public int getColumnCount() {
            return 1;
        }

        /**
         * Returns the value at a specific row and column.
         */
        public Object getValueAt(int row, int col) {
            return super.getValueAt(row, col + 1);
        }

        /**
         * Returns true of this table cells are editable, false -
         * otherwise.
         */
        public boolean isCellEditable(int row, int col) {
            return super.isCellEditable(row, col + 1);
        }

    }

    public class ValuesOnlyPointedTableCellRenderer extends PointedMemoryTableCellRenderer {

        private static final long serialVersionUID = -1118035116970752400L;

        public void setRenderer(int row, int column) {
            super.setRenderer(row, column + 1);
        }
    }
}
